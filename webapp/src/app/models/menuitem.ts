export class ChildMenuItem {
    routerLink: string;
    displayName: string;
}

export class MenuItem {
    routerLink: string;
    displayName: string;
    title: string;
    iconClass: string;
    children?: ChildMenuItem[];
}
