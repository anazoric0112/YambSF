import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { YambService } from '../services/yamb.service';

@Component({
  selector: 'app-simulation',
  standalone: true,
  imports: [],
  templateUrl: './simulation.component.html',
  styleUrl: './simulation.component.css'
})
export class SimulationComponent {
    constructor(public router: Router, private service: YambService){}

    result: string ="        ";
    playgame(){
        let id = Number.parseInt(localStorage.getItem("id")!);
        this.result = "Loading..."
        this.service.playWholeGame(id).subscribe(
            data =>{
                this.result=data.toString();
            }
        )
    }
}
