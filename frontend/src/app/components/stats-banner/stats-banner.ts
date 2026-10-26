import { Component } from '@angular/core';

interface StatItem {
  icon: 'flask' | 'team' | 'clock' | 'map';
  value: string;
  label: string;
}

@Component({
  selector: 'app-stats-banner',
  templateUrl: './stats-banner.html',
  styleUrl: './stats-banner.css',
})
export class StatsBanner {
  protected readonly stats: StatItem[] = [
    {
      icon: 'flask',
      value: '500+',
      label: 'Diagnostic Tests',
    },
    {
      icon: 'team',
      value: 'Experienced',
      label: 'Professional Team',
    },
    {
      icon: 'clock',
      value: '24/7',
      label: 'Laboratory Services',
    },
    {
      icon: 'map',
      value: 'Multiple',
      label: 'Branch Locations',
    },
  ];
}
