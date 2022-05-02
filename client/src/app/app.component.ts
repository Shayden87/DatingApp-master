import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';
import { PresenceService } from './_services/presence.service';

/**
 * Decorator that transforms class into an angular component.
 * 
 * @remarks
 * Includes two separate files {html/css} to create the {template/style} 
 */
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
/**
 * Class to verify and perform HTTP requests
 */
export class AppComponent implements OnInit {
  title = 'The Dating App';
  users: any;
  /**
   * Constructor for verifying availability of HTTP requests
   * @param http 
   * Dependency injection of the HttpClient class
   */
  constructor(private accountService: AccountService, private presence: PresenceService) {}

  ngOnInit() {
    this.setCurrentUser();
  }

  // Sets current user.
  setCurrentUser() {
    const user: User = JSON.parse(localStorage.getItem('user'));
    if (user) {
      this.accountService.setCurrentUser(user);
      this.presence.createHubConnection(user);
    }
    
  }

}
