export class MerchantInfo {
    id: string;
    companyName: string;
    cvrNumber: string;
    refundPercentage: number;
    addressStreetName: string;
    addressStreetNumber: string;
    addressPostalCode: string;
    addressCity: string;
    addressCountry: string;
    latitude?: number;
    longitude?: number;
    description: string;
    banner: string;
    logo: string;
    vatNumber: string;
    contactEmail: string;
    contactPhone: string;
    currency: string;
    openingHours: OpeningHours[];
    tags: number[];
}

class OpeningHours {
    day: number;
    open: string;
    close: string;
}


