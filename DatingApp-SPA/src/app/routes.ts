import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MemberListComponent } from './member-list/member-list.component';
import { MessagesComponent } from './messages/messages.component';
import { ListsComponent } from './lists/lists.component';
import { AuthGuard } from './_guards/auth.guard';

/*
Add routing paths from nav bar
Add Route guards from unauthorized users
allow anon access to home (only)
*/
export const appRoutes: Routes = [
    {   path: '', component: HomeComponent  },
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [AuthGuard],
        children:   [
            {   path: 'members', component: MemberListComponent },
            {   path: 'messages', component: MessagesComponent  },
            {   path: 'lists', component: ListsComponent  }
        ]
    },
    {   path: '**', redirectTo: 'home', pathMatch: 'full'}

];
