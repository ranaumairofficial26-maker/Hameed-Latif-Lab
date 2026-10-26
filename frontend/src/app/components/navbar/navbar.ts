import { Component, HostListener, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

interface NavChild {
  label: string;
  link: string;
}

interface NavItem {
  label: string;
  link: string;
  children?: NavChild[];
}

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  protected readonly phone = '+92 311 1222873';
  protected readonly phoneHref = 'tel:+923111222873';
  protected readonly email = 'info@hameedlatifhospitallabs.com';
  protected readonly address = '14 - Abu Bakar Block, New Garden Town, Lahore - Pakistan';

  protected readonly scrolled = signal(false);
  protected readonly mobileOpen = signal(false);
  protected readonly openDropdown = signal<string | null>(null);

  protected readonly navItems: NavItem[] = [
    { label: 'Home', link: '/' },
    {
      label: 'About Us',
      link: '/about',
      children: [
        { label: 'Our History', link: '/about/history' },
        { label: 'Mission & Vision', link: '/about/mission' },
        { label: 'Leadership', link: '/about/leadership' },
        { label: 'Accreditations', link: '/about/accreditations' },
      ],
    },
    {
      label: 'Departments',
      link: '/departments',
      children: [
        { label: 'Hematology', link: '/departments/hematology' },
        { label: 'Clinical Chemistry', link: '/departments/chemistry' },
        { label: 'Microbiology', link: '/departments/microbiology' },
        { label: 'Histopathology', link: '/departments/histopathology' },
        { label: 'Molecular Diagnostics', link: '/departments/molecular' },
        { label: 'Radiology', link: '/departments/radiology' },
      ],
    },
    {
      label: 'Tests',
      link: '/tests',
      children: [
        { label: 'Test Directory', link: '/tests' },
        { label: 'Health Packages', link: '/tests/packages' },
        { label: 'Test Preparation', link: '/tests/preparation' },
      ],
    },
    {
      label: 'Services',
      link: '/services',
      children: [
        { label: 'Home Sampling', link: '/services/home-sampling' },
        { label: 'Corporate Services', link: '/services/corporate' },
        { label: 'Online Reports', link: '/services/reports' },
        { label: 'Collection Centers', link: '/services/centers' },
      ],
    },
    {
      label: 'Contact Us',
      link: '/contact',
      children: [
        { label: 'Our Branches', link: '/contact/branches' },
        { label: 'Feedback', link: '/contact/feedback' },
        { label: 'Careers', link: '/contact/careers' },
      ],
    },
    {
      label: 'COVID19 Test',
      link: '/covid',
      children: [
        { label: 'PCR Test', link: '/covid/pcr' },
        { label: 'Rapid Antigen Test', link: '/covid/antigen' },
        { label: 'Antibody Test', link: '/covid/antibody' },
        { label: 'Travel Testing', link: '/covid/travel' },
      ],
    },
  ];

  @HostListener('window:scroll')
  onScroll(): void {
    this.scrolled.set(window.scrollY > 40);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement | null;
    if (!target?.closest('.nav-item')) {
      this.openDropdown.set(null);
    }
  }

  toggleMobile(): void {
    this.mobileOpen.update((v) => !v);
    this.openDropdown.set(null);
  }

  closeMobile(): void {
    this.mobileOpen.set(false);
    this.openDropdown.set(null);
  }

  toggleDropdown(label: string): void {
    this.openDropdown.update((cur) => (cur === label ? null : label));
  }
}
