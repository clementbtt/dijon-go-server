import { Component, AfterViewInit, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute } from '@angular/router';
import { WebsocketService } from '../websocket.service';
import { UserCookieService } from '../Model/UserCookieService';

@Component({
  selector: 'app-grid',
  standalone: true,
  imports: [NgFor, NgIf, MatIconModule],
  templateUrl: './grid.component.html',
  styleUrl: './grid.component.css',
})
export class GridComponent implements AfterViewInit, OnInit {
  private size: number;
  private opponentAvatar: string;
  private opponentPseudo: string;
  private playerAvatar: string;
  private playerPseudo: string;

  public get PlayerAvatar() {
    return this.playerAvatar;
  }

  public get PlayerPseudo() {
    return this.playerPseudo;
  }

  public constructor(
    private websocketService: WebsocketService,
    private userCookieService: UserCookieService
  ) {
    this.size = 0;
    this.opponentAvatar = '';
    this.opponentPseudo = '';
    this.playerPseudo = this.userCookieService.getUser().Username; // Récupère le nom d'utilisateur et l'avatar pour l'afficher sur la page
    this.playerAvatar =
      'https://localhost:7065/profile-pics/' + this.playerPseudo;
  }

  public getSize(): number {
    return this.size - 1;
  }

  ngOnInit(): void {
    this.size = 19;
  }

  ngAfterViewInit(): void {
    let stones = document.getElementsByClassName('stone');
    let stonesArray = Array.from(stones);
    stonesArray.forEach((stone) => {
      stone.addEventListener('click', () => {
        this.click(stone);
      });
    });

    let passButton = document.getElementById('pass');
    passButton?.addEventListener('click', () => {
      this.skipTurn();
    });
  }

  public click(stone: any): void {
    this.websocketService.placeStone(stone.id);
  }

  public skipTurn() {
    this.websocketService.skipTurn();
  }
}