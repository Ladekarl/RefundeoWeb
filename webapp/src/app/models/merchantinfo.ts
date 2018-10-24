import {AttachedAccount} from './attachedaccount';

export class MerchantInfo {
    id: string;
    username: string;
    password: string;
    companyName: string;
    cvrNumber: string;
    vatRate: number;
    priceLevel: number;
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
    adminEmail: string;
    currency: string;
    openingHours: OpeningHours[];
    attachedAccounts: AttachedAccount[];
    tags: number[];
    feePoints: FeePoint[];
}

export class OpeningHours {
    day: number;
    open: string;
    close: string;
}

export class FeePoint {
    end?: number;
    start: number;
    merchantFee: number;
    adminFee: number;
    refundPercentage: number;
}


