import {MerchantInfo} from './merchantinfo';
import {CustomerInfo} from './customerinfo';

export class RefundCase {
    id: number;
    amount: number;
    refundAmount: number;
    isRequested: boolean;
    isAccepted: boolean;
    isRejected: boolean;
    receiptNumber: string;
    receiptImage: string;
    vatFormImage: string;
    checked: boolean;
    dateCreated: Date;
    dateRequested: Date;
    merchant?: MerchantInfo;
    customer?: CustomerInfo;
}
