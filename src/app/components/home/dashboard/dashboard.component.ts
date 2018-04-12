import { Component, OnInit } from '@angular/core';
import { User, RefundCase } from '../../../models/index';
import { UserService, RefundCasesService, AuthenticationService, ColorsService } from '../../../services/index';
import { CurrentUser } from '../../../models';

@Component({
  selector: 'app-dashboard',
  templateUrl: 'dashboard.component.html',
  styleUrls: ['dashboard.component.scss']
})

export class DashboardComponent implements OnInit {
  currentUser: CurrentUser;
  usersByCountry: any;

  constructor(private authenticationService: AuthenticationService, private refundCasesService: RefundCasesService,
    private colorsService: ColorsService) {
  }

  ngOnInit(): void {
    this.refundCasesService.getAll().subscribe(refundCases => {
      const usersByCountryMap = this.getUsersByCountry(refundCases);
      const countries = Array.from(usersByCountryMap.keys());
      const amounts = Array.from(usersByCountryMap.values());
      const colorPalette = this.colorsService.getColorPallete(usersByCountryMap.keys.length);
      this.usersByCountry = {
        labels: countries,
        datasets: [
          {
            backgroundColor: colorPalette,
            hoverBackgroundColor: colorPalette,
            data: amounts
          }]
      };
    });
  }

  getUsersByCountry(refundCases: RefundCase[]): Map<string, number> {
    const usersByCountryMap = new Map<string, number>();
    refundCases.forEach(r => {
      if (r.customer) {
        let amount: number = usersByCountryMap.get(r.customer.country);
        amount = amount ? ++amount : 1;
        usersByCountryMap.set(r.customer.country, amount);
      }
    });
    return usersByCountryMap;
  }
}
