import { AfterViewInit, Component, ElementRef, OnInit } from '@angular/core';
import { SwaggerService } from '../services/index';

@Component({
  selector: 'app-swagger',
  templateUrl: './swagger.component.html',
  styleUrls: ['swagger.component.scss']
})
export class SwaggerComponent implements OnInit, AfterViewInit {

  constructor(private el: ElementRef, private swaggerService: SwaggerService) {
  }

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.swaggerService.getSwagger(this.el.nativeElement.querySelector('.swagger-container'));
  }
}
