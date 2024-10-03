import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { YambService } from '../services/yamb.service';

@Component({
  selector: 'app-homepage',
  standalone: true,
  imports: [ CommonModule],
  templateUrl: './homepage.component.html',
  styleUrl: './homepage.component.css'
})
export class HomepageComponent implements OnInit{

    constructor(public router: Router, private service: YambService){}

    ngOnInit(): void {
        var id = localStorage.getItem("id");
        if (id!=null) this.id = Number.parseInt(id);
        
        //retrieve game if it exists
        this.service.sheet(this.id).subscribe(
            data=>{
                for (let i=0;i<data.length;i++){
                    for (let j=0;j<data[i].length;j++){
                        if (data[i][j]==-1) continue;
                        this.sheet[j][i] = data[i][j].toString();
                        this.moves+=1;
                        this.sum[i]+=data[i][j];
                        if (i==0 && j+1==6
                            || i==1 && j+1==1) 
                            this.sum[i]+=30
                    }
                }
            }
        )
    }

    savedIndexes: number[]=[];
    tempDice: string[] = ["","","","","",""];
    availableNumbers: number[] = [1,2,3,4,5,6];
    throws: number = 0;
    moves: number = 0;
    id: number = 0;
    sum: number[]=[0,0];

    sheet: string[][] = [[" "," "],[" "," "],[" "," "],[" "," "],[" "," "],[" "," "]];

    writeTo(column: number, row: number){
        if (this.throws==0) return;

        let target = row;
        let cnt = 0;
        for (let i=0;i<this.tempDice.length;i++){
            if (this.tempDice[i]==target.toString()) cnt+=1;
        }
        if (cnt>5) cnt = 5;

        this.service.addMove(this.id,cnt,target, column==0?"down":"up").subscribe(
            data=>{
                this.sheet[row-1][column]=(target*cnt).toString();
                this.throws=0;
                this.savedIndexes = [];
                this.tempDice = ["","","","","",""];
                this.moves+=1;
                
                this.sum[column]+=target*cnt;
                if ( this.sum[column]>=60 && 
                    (column==0 && target==6
                    || column==1 && target==1)) 
                    this.sum[column]+=30
            }
        );
    }

    toggleSavedDice(ind: number){
        if (this.tempDice[ind]=="") return;

        for (let i=0;i<this.savedIndexes.length;i++){
            if (this.savedIndexes[i]==ind) {
                this.savedIndexes.splice(i,1);
                return;
            }
        }
        if (this.savedIndexes.length==5) return;
        this.savedIndexes.push(ind);
    }

    startGame(){
        this.service.startGame(this.id).subscribe(
            data=>{
                this.sheet = [[" "," "],[" "," "],[" "," "],[" "," "],[" "," "],[" "," "]];
                this.moves = 0;
                this.sum = [0,0];
            }
        );
    }

    throwDice(){
        this.service.throwDice(this.id).subscribe(
            data=>{
                let row1 = data[0]
                for (let i=0;i<6;i++){
                    if (this.isSavedDice(i)) continue;
                    this.tempDice[i]=row1[i].toString();
                }
                this.throws+=1;
            }
        );
    }

    isSavedDice(ind: number):boolean{
        for (let i=0;i<this.savedIndexes.length;i++)
            if (this.savedIndexes[i]==ind) return true;
        return false;
    }

    isNext(column: number, row: number): boolean{
        let ind1=-1, ind2=-1;
        for (let i=0;i<6;i++){
            if (this.sheet[i][0]!=" ") continue;
            ind1=i; break;
        }
        for (let i=5;i>=0;i--){
            if (this.sheet[i][1]!=" ") continue;
            ind2=i; break;
        }
        return (column==0 && row==ind1+1) 
                || (column==1 && row==ind2+1);
    }

    logout(){
        localStorage.removeItem("id");
        this.router.navigate(["login"])
    }
}
