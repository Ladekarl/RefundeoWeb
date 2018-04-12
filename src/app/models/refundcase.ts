import { Customer } from './customer';
import { Merchant } from './merchant';

export class RefundCase {
    id: number;
    amount: number;
    refundAmount: number;
    isRequested: boolean;
    isAccepted: boolean;
    documentation?: string;
    dateCreated: string;
    dateRequested: string;
    merchant?: Merchant;
    customer?: Customer;
}
