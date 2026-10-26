import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './components/navbar/navbar';
import { Hero } from './components/hero/hero';
import { StatsBanner } from './components/stats-banner/stats-banner';
import { About } from './components/about/about';
import { Features } from './components/features/features';
import { Branches } from './components/branches/branches';
import { Partners } from './components/partners/partners';
import { CtaBanner } from './components/cta-banner/cta-banner';
import { Footer } from './components/footer/footer';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, Hero, StatsBanner, About, Features, Branches, Partners, CtaBanner, Footer],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {}
