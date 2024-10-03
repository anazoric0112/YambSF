import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [],
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.css'
})
export class MenuComponent {
    constructor(public router: Router){}
    
    logout(){
        localStorage.removeItem("id");
        this.router.navigate(["login"])
    }
}
