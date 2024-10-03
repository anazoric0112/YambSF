import { Routes } from '@angular/router';
import { HomepageComponent } from './homepage/homepage.component';
import { LoginComponent } from './login/login.component';
import { LeaderboardComponent } from './leaderboard/leaderboard.component';
import { MenuComponent } from './menu/menu.component';
import { SimulationComponent } from './simulation/simulation.component';

export const routes: Routes = [
    {path:"", component: LoginComponent},
    {path:"game", component: HomepageComponent},
    {path:"login", component: LoginComponent},
    {path:'leaderboard', component: LeaderboardComponent},
    {path:'menu', component: MenuComponent},
    {path:'simulation', component: SimulationComponent},
];
