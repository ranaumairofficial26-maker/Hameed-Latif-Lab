import { Component } from '@angular/core';

export interface AirlinePartner {
  id: string;
  name: string;
  logo: string;
  description: string;
}

@Component({
  selector: 'app-partners',
  templateUrl: './partners.html',
  styleUrl: './partners.css',
})
export class Partners {
  protected readonly airlines: AirlinePartner[] = [
    {
      id: 'british-airways',
      name: 'British Airways',
      logo: 'images/partners/british_airways.png?v=2',
      description: 'Official Travel Diagnostic Partner',
    },
    {
      id: 'pia',
      name: 'Pakistan International Airlines',
      logo: 'images/partners/pia.png?v=2',
      description: 'National Carrier Partner',
    },
    {
      id: 'emirates',
      name: 'Emirates',
      logo: 'images/partners/emirates.png?v=2',
      description: 'Global Travel Health Partner',
    },
    {
      id: 'turkish-airlines',
      name: 'Turkish Airlines',
      logo: 'images/partners/turkish_airlines.png?v=2',
      description: 'International Diagnostic Partner',
    },
    {
      id: 'qatar-airways',
      name: 'Qatar Airways',
      logo: 'images/partners/qatar_airways.png?v=2',
      description: 'Pre-Departure Testing Partner',
    },
  ];
}
