import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class YambService {

    constructor(private http: HttpClient) { }

    //will be changed when I finalize autoscaling and entry service
    url: string = "http://localhost:8781/api/YambPlatform";

    //-------Game endpoint calls-------
    startGame (id: number){
        return this.http.post<string>(`${this.url}/startgame/${id}`,{});
    }

    //where must have values "up" or "down"
    addMove (id: number, cnt: number, target: number, where: string){
        return this.http.post<string>(`${this.url}/addmove/${id}/${cnt}/${target}/${where}`,{});
    }
    
    throwDice (id: number){
        return this.http.get<number[][]>(`${this.url}/throwdice/${id}`);
    }

    sheet (id: number){
        return this.http.get<number[][]>(`${this.url}/sheet/${id}`);
    }

    //-------Leaderboard endpoint calls-------
    highscore (id: number){
        return this.http.get<number>(`${this.url}/highscore/${id}`);
    }

    averagescore (id: number){
        return this.http.get<number>(`${this.url}/averagescore/${id}`);
    }

    ranking (id: number){
        return this.http.get<number>(`${this.url}/ranking/${id}`);
    }
    
    leaderboard (){
        return this.http.get<number[][]>(`${this.url}/leaderboard`);
    }

    //-------Testmode endpoint calls-------
    playWholeGame (id: number){
        return this.http.post<number>(`${this.url}/playgame/${id}`,{});
    }
}
