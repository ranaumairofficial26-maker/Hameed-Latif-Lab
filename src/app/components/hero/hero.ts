import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface HeroFeature {
  icon: string;
  line1: string;
  line2: string;
}

@Component({
  selector: 'app-hero',
  imports: [RouterLink],
  templateUrl: './hero.html',
  styleUrl: './hero.css',
})
export class Hero {
  protected readonly features: HeroFeature[] = [
    { icon: 'shield-check', line1: 'Accurate', line2: 'Results' },
    { icon: 'clock', line1: 'Fast', line2: 'Turnaround Time' },
    { icon: 'cpu', line1: 'Advanced', line2: 'Technology' },
    { icon: 'users', line1: 'Experienced', line2: 'Professional Team' },
  ];
}
