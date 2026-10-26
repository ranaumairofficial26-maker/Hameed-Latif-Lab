import { Component } from '@angular/core';

interface ProcessStep {
  id: string;
  icon: 'microscope' | 'chip' | 'team' | 'clock' | 'headset';
  line1: string;
  line2: string;
}

@Component({
  selector: 'app-features',
  templateUrl: './features.html',
  styleUrl: './features.css',
})
export class Features {
  protected readonly steps: ProcessStep[] = [
    {
      id: 'step-1',
      icon: 'microscope',
      line1: 'Quality',
      line2: 'Assurance',
    },
    {
      id: 'step-2',
      icon: 'chip',
      line1: 'Modern',
      line2: 'Technology',
    },
    {
      id: 'step-3',
      icon: 'team',
      line1: 'Experienced',
      line2: 'Team',
    },
    {
      id: 'step-4',
      icon: 'clock',
      line1: 'Timely',
      line2: 'Reporting',
    },
    {
      id: 'step-5',
      icon: 'headset',
      line1: 'Patient',
      line2: 'Support',
    },
  ];
}
