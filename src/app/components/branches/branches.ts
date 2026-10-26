import { Component } from '@angular/core';

export interface BranchLocation {
  id: string;
  name: string;
  address: string;
  area: string;
  badge: string;
  image: string;
  timings: string;
  phone: string;
  mapsUrl: string;
}

@Component({
  selector: 'app-branches',
  templateUrl: './branches.html',
  styleUrl: './branches.css',
})
export class Branches {
  protected readonly branches: BranchLocation[] = [
    {
      id: 'main-branch',
      name: 'Main Hospital',
      address: '14 - Abu Bakar Block, New Garden Town, Lahore',
      area: 'Ferozepur Road',
      badge: 'Main Campus • 24/7',
      image: 'images/branches/main_branch.png',
      timings: '24 Hours / 7 Days',
      phone: '+92 311 1222873',
      mapsUrl: 'https://maps.google.com/?q=Hameed+Latif+Hospital+Lahore',
    },
    {
      id: 'misri-shah-branch',
      name: 'Misri Shah Branch',
      address: 'Near Shadbagh Main Road, Misri Shah, Lahore',
      area: 'Misri Shah',
      badge: 'Collection & Diagnostics',
      image: 'images/branches/misri_shah.jpg',
      timings: '8:00 AM - 10:00 PM',
      phone: '+92 42 37602345',
      mapsUrl: 'https://maps.google.com/?q=Hameed+Latif+Hospital+Misri+Shah+Lahore',
    },
    {
      id: 'dha-phase5-branch',
      name: 'Hameed Latif DHA Phase 5',
      address: 'Main Commercial Plaza, Phase 5 DHA, Lahore',
      area: 'DHA Phase 5',
      badge: 'Executive Laboratory',
      image: 'images/branches/dha_phase5.png?v=2',
      timings: '8:00 AM - 11:00 PM',
      phone: '+92 42 35748900',
      mapsUrl: 'https://maps.google.com/?q=Hameed+Latif+Hospital+DHA+Phase+5+Lahore',
    },
  ];
}
