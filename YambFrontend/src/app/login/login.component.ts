import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms'
import { YambService } from '../services/yamb.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {

    constructor(private router: Router, private service: YambService){ }

    id: string="";

    login(){
        if (this.id=="") return;
        localStorage.setItem("id",this.id);
        this.router.navigate(["menu"]);
    }
}
