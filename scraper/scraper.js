const puppeteer = require('puppeteer');
const OpenLocationCode = require('open-location-code').OpenLocationCode;
const openLocationCode = new OpenLocationCode();
const fs = require('fs');

process.setMaxListeners(0);
const defaultTimeout = 10000;
const queryString = 'https://www.google.ca/maps/search/shops+in+';
let pageLimit = 2;

let launchBrowser = () => {
	return puppeteer.launch({
		args: ['--no-sandbox', '--disabled-setuid-sandbox', '--enable-feature=NetworkService'], 
		headless: true,
		ignoreHTTPSErrors: true
	});
}

let scrape = async (pagesToScrape) => {
	let result = [];
	let browser;
	try {
		browser = await launchBrowser();
		const page = await browser.newPage();

		for(let i = 0; i < pagesToScrape.length; i++) {
			const pageName = pagesToScrape[i];
			const pageToScrape = queryString + pageName;
			let props = [];
			try {
				console.log('Opening page ' + pageToScrape);
				await page.goto(pageToScrape);
				let propsPromises = [];
				let resultPage = 0;
				let isDisabled;
				do {
					try {
						const elements = await page.$$('.section-result');
						propsPromises.push(
							getPropsFromSections(pageToScrape, elements, resultPage)
						);
						let buttonHandle = await page.$('#section-pagination-button-next');
						let isDisabledHandle = await buttonHandle.getProperty('disabled');
						isDisabled = isDisabledHandle ? await isDisabledHandle.jsonValue() : true;
						if(!isDisabled) {
							await page.waitFor(() => !document.querySelector('.early-pane-overlay'), {timeout: defaultTimeout});
							await page.click('#section-pagination-button-next');
							resultPage++;
							try {
								await page.waitFor(() => document.querySelector('.section-refresh-overlay-visible'), {timeout: defaultTimeout});
								await page.waitFor(() => !document.querySelector('.section-refresh-overlay-visible'), {timeout: defaultTimeout});
							} catch(err) {
								console.warn('Error in waiting for next page: ' + (resultPage + 1));
							}
						}
					} catch(err) {
						console.log('An error occured: ' + err);
					}

					if(isDisabled || propsPromises.length >= pageLimit) {
						if(isDisabled) console.log('-- No more pages. Awaiting results.');
						else console.log('-- Concurrent page limit ' + pageLimit + ' reached. Awaiting results.');
						let res = await Promise.all(propsPromises).then((res) => [].concat.apply([], res));

						res = res
							.filter(obj => typeof obj.companyName != "undefined")
							.filter((obj, pos, arr) => arr
								.map(mapObj => mapObj['companyName']
								.indexOf(obj['companyName']) === pos));
		
						res.forEach(shop => {
							if(shop.plusCode) {
								const codes = shop.plusCode.trim().split(' ');
								let fullCode;
								if(codes.length > 1) {
									const shortCode = codes[0].trim();
									if(shop.latitude && shop.longitude) {
										fullCode = openLocationCode.recoverNearest(shortCode, shop.latitude, shop.longitude);	
									}
									delete shop.latitude;
									delete shop.longitude;
								} else {
									fullCode = codes[0];
								}
								if(fullCode) {
									const coord = openLocationCode.decode(fullCode);
									if(coord) {
										shop.latitude = coord.latitudeCenter;
										shop.longitude = coord.longitudeCenter;
									}
								}
							}
							delete shop.plusCode;
						});

						props.push(res);
						props = [].concat.apply([], props);
						propsPromises = [];

						if(filePath) {
							fs.appendFile(filePath + pageName + "_" + resultPage + ".json", JSON.stringify(res), (err) => {  
								if (err) throw err;
							});
						}
					}
				} while(!isDisabled && !page.isClosed())

				result.push({
					city: pageName,
					shops: props
				});
			} catch(err) {
				result.push({
					city: pageName,
					error: err
				});
			}
		}
		browser.close();
	} catch(err) {
		if(browser) {
			browser.close();
		}
		return err.toString();
	}
	return result;
};

let getPropsFromSections = async function(pageToScrape, elements, resultPage) {
	let pagePromises = [];
	console.log("-- Scraping page " + (resultPage + 1));
	for(let j = 0; j < elements.length; j++) {
		let browser = await launchBrowser();
		if(pagePromises.length > 1) {
			 await Promise.race(pagePromises);
		} 
		pagePromises.push(browser.newPage().then(async (page) => {
			page.setDefaultNavigationTimeout(defaultTimeout);
			try {
				try {
					await page.goto(pageToScrape, {timeout: defaultTimeout * 10 });
					await page.waitFor(1000);
				} catch(err) {
					throw {
						error: err.toString(),
						errorMessage : 'Error in tab goto ' + pageToScrape
					}
				}
				for(let i = 0; i < resultPage; i++) {
					try {
						await page.evaluate(() => {
							const elem = document.querySelector('#section-pagination-button-next');
							if(elem)
								elem.click();
						});
					} catch(err) {
						throw {
							error: err.toString(),
							errorMessage : 'Error in tab going to next page ' + (i + 2)
						}
					}
					try {
						await page.waitFor(() => document.querySelector('.section-refresh-overlay-visible'), {timeout: defaultTimeout});
						await page.waitFor(() => !document.querySelector('.section-refresh-overlay-visible'), {timeout: defaultTimeout});
					} catch(err) {
						console.warn('Timed out waiting for next page ' + (i + 2) + ' : ' + err.toString()); 
					}
				}
				try {
					await page.evaluate((index) => {
						const elem = document.querySelectorAll('.section-result')[index];
						if(elem)
							elem.click();
					}, j);
				} catch(err) {
					throw {
						error: err.toString(),
						errorMessage : 'Error in tab clicking section ' + j + ' of page ' + (resultPage + 1)
					}
				}
				try {
					await page.waitFor(() => document.querySelector('.section-hero-header-title'), {timeout: defaultTimeout});
				} catch(err) {
					console.warn('Timed out when waiting for click: ' + err.toString());
				}
				let results;
				try {
					var retries = 0;
					while(!results && retries < 5) {
						results = await page.evaluate(extractProps);
						if(!results) {
							await page.waitFor(200);	
						}
						retries++;
					}
				} catch(err) {
					throw {
						error: err.toString(),
						errorMessage : 'Error in tab evaluating section ' + j + ' of page ' + (resultPage + 1)
					}
				}
				if(!results) {
					throw {
						error: 'Could not retrieve props',
						errorMessage: 'Failed to retrieve the props of section ' + j + ' of page ' + (resultPage + 1)
					}
				}
				await page.close();
				return results;
			} catch(err) {
				if(page && !page.isClosed()) {
					await page.close();
				}
				return err;
			}
		}).finally((res) => {
			browser.close();
			return res;
		}));
	}
	return Promise.all(pagePromises).then((res) => {
		console.log("-- Finished scraping page " + (resultPage + 1));
		return res;
	});
}

let extractProps = function() {

	let findElementInSection = (searchString) => {
		const sections = document.querySelectorAll('.section-info-line');
		let match;
		for(let i = 0; i < sections.length; i++) {
			const section = sections[i];
			if(section.innerHTML.indexOf(searchString) > -1) {
				match = section;
			}
		}
		return match;
	}

	let extractProp = (element, prop) => element ? element[prop] : element;

	let findOpeningHours = () => {		
		let convertTime12to24 = (time12h) => {
			if(!time12h) return;
			const timeArr = time12h.split(/(?=a|p)/);
			const time = timeArr[0];
			let modifier;
			if(timeArr.length > 1) {
				modifier = timeArr[1];
			}
			let [hours, minutes] = time.split(':');
			if (hours === '12') {
			  hours = '00';
			}
			if (modifier && modifier.indexOf('p') > -1) {
			  hours = (parseInt(hours, 10) + 12).toString();
			}
			return (hours.length === 1 ? '0' + hours : hours) + ':' + (minutes ? minutes : '00');
		  }

		const rows = document.querySelectorAll('.widget-pane-info-open-hours-row-table-hoverable tbody tr');
		let openingHours;
		for(let i = 0; i < rows.length; i++) {
			const row = rows[i];
			if(row && row.firstElementChild) {
				let day;
				let dayText = row.firstElementChild.innerText;

				if(dayText.indexOf('Sunday') > -1) {
					day = 0;
				} else if(dayText.indexOf('Monday') > -1) {
					day = 1;
				} else if(dayText.indexOf('Tuesday') > -1) { 
					day = 2;
				} else if(dayText.indexOf('Wednesday') > -1) {
					day = 3;
				} else if(dayText.indexOf('Thursday') > -1) { 
					day = 4;
				} else if(dayText.indexOf('Friday') > -1) { 
					day = 5;
				} else if(dayText.indexOf('Saturday') > -1) { 
					day = 6;
				}

				if(day) {
					if(!openingHours) {
						openingHours = [];
					}

					const openingHoursText = row.children[1].innerText.trim();
					if(openingHoursText.toLowerCase().indexOf('closed') > -1) {
						continue;
					}
					const openingHoursArr = openingHoursText.split(' ')[0].split('–');
					let open, close;
					if(openingHoursText.indexOf('a.m') > -1 || openingHoursText.indexOf('p.m') > -1) {
						open = convertTime12to24(openingHoursArr[0]);
						close = convertTime12to24(openingHoursArr[1]);
					} else if(openingHoursText.toLowerCase().indexOf('open 24 hours') > -1) {
						open = '00:00';
						close = '23:59';
					} else {
						open = openingHoursArr[0];
						close = openingHoursArr[1];
					}
					openingHours.push({
						day,
						open,
						close
					});
				}
			}
		}
		 return openingHours;
	}

	let findAddress = (addressProp) => {
		let addressStreetName,
			addressStreetNumber,
			addressPostalCode,
			addressCity,
			addressCountry,
			addressStreetNameNumber,
			addressPostalCodeCity;
		const address = extractProp(findElementInSection('maps-sprite-pane-info-address'), 'innerText');
		if(address) {
			[addressStreetNameNumber, addressPostalCodeCity, addressCountry] = address.trim().split(',');
			if(addressStreetNameNumber) {
				[addressStreetName, addressStreetNumber] = addressStreetNameNumber.trim().split(' ');
			}
			if(addressPostalCodeCity) {
				[addressPostalCode, addressCity] = addressPostalCodeCity.trim().split(' ');
			}
			if(addressCountry) {
				addressCountry = addressCountry.trim();
			}
		}
		let result = {
			addressStreetName,
			addressStreetNumber,
			addressPostalCode,
			addressCity,
			addressCountry
		}
		return result[addressProp];
	}

	let findLatLong = (latLongProp) => {
		const metas = document.querySelectorAll('meta');
		let foundMeta;
		metas.forEach(meta => {
			if(meta.content && meta.content.indexOf('center=') > -1) foundMeta = meta; 
		});
		if(foundMeta && foundMeta.content) {
			const latLongExtra = foundMeta.content.split('center=')
			if(latLongExtra && latLongExtra.length > 1) {
				const latLongZ = latLongExtra[1].split('&')
				if(latLongZ && latLongZ.length > 1) {
					latLong = latLongZ[0].split('%2C');
					if(latLong && latLong.length > 1) {
						const latitude = latLong[0];
						if(latitude && latLongProp === 'latitude') 
							return parseFloat(latitude)
						const longitude = latLong[1];
						if(longitude && latLongProp === 'longitude') 
							return parseFloat(longitude);
					}
				}
			}
		}
	}

	let propsToScrape = {
		companyName: () => extractProp(document.querySelector('.section-hero-header-title'), 'innerText'),
		addressStreetName: () => findAddress('addressStreetName'),
		addressStreetNumber: () => findAddress('addressStreetNumber'),
		addressPostalCode: () => findAddress('addressPostalCode'),
		addressCity: () => findAddress('addressCity'),
		addressCountry: () => findAddress('addressCountry'),
		website: () => extractProp(findElementInSection('maps-sprite-pane-info-website'), 'innerText'),
		phone: () => extractProp(findElementInSection('maps-sprite-pane-info-phone'), 'innerText'),
		banner: () => {
			const img = extractProp(document.querySelector('.section-hero-header-hero img'), 'src');
			return img.indexOf('maps.gstatic.com') > -1 ? undefined : img;
		},
		rating: () => parseFloat(extractProp(document.querySelector('.section-star-display'), 'innerText')),
		tag: () => extractProp(extractProp(document.querySelector('.section-rating'), 'lastElementChild'), 'innerText'),
		openingHours: findOpeningHours,
		latitude: () => findLatLong('latitude'),
		longitude: () => findLatLong('longitude'),
		plusCode: () => extractProp(findElementInSection('maps-sprite-pane-info-plus-code'), 'innerText')
	}

	let propNames = Object.getOwnPropertyNames(propsToScrape);
	let scrapeResult;

	for(let i = 0; i < propNames.length; i++) {
		let propName = propNames[i];
		let query = propsToScrape[propName];
		let queryResult = query(document);
		if(queryResult) {
			if(!scrapeResult) {
				scrapeResult = {};
			}
			scrapeResult[propName] = queryResult;
		}
	}

	return scrapeResult;
}

hasDebug = false;
let filePath;
process.argv.forEach((val, index) => {
	if(!val) return;
	if(val === 'debug') {
		hasDebug = true;
	}
	else if(val.indexOf('p=') > -1) {
		filePath = val.split('p=')[1];
	} else if (val.indexOf('l=') > -1) {
		pageLimit = parseInt(val.split('l=')[1]);
	}
});

if(!hasDebug) {
	console.warn = () => {};
} else {
	console.log('Debug enabled');
}

scrape([
	//'Hinnerup', 
	//'Lystrup',
	//'Aarhus',
	'grundfør'
]).then((scrapedCities) => {
	scrapedCities.forEach(result => {
		if(result) {
			if(result.error) {
				console.log("Error: ");
				console.log(result.error);
				return;
			}
		}
	});
}, (err) => {
	console.log(err);
});