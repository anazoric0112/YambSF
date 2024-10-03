import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { YambService } from '../services/yamb.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './leaderboard.component.html',
  styleUrl: './leaderboard.component.css'
})
export class LeaderboardComponent implements OnInit {

    constructor(public router: Router, private service: YambService){}

    ngOnInit(): void {
        var id = localStorage.getItem("id");
        if (id!=null) this.id = JSON.parse(id);

        this.service.highscore(this.id).subscribe(
            data => { this.highscore = data; this.loaded+=1;}
        );
        this.service.averagescore(this.id).subscribe(
            data => { this.averagescore = data; this.loaded+=1;}
        );
        this.service.ranking(this.id).subscribe(
            data => { this.ranking = data; this.loaded+=1;}
        );
        this.service.leaderboard().subscribe(
            data => {this.leaderboard = data; this.loaded+=1;}
        )
    }

    id: number = 0;
    highscore: number = 0;
    averagescore: number = 0;
    ranking: number = 0;
    leaderboard: number[][] = [];
    loaded: number = 0;

    logout(){
        localStorage.removeItem("id");
        this.router.navigate(["login"])
    }
}
