import { Component, afterNextRender } from '@angular/core';
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
import { ChatbotWidget } from './components/chatbot-widget/chatbot-widget';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, Hero, StatsBanner, About, Features, Branches, Partners, CtaBanner, Footer, ChatbotWidget],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  constructor() {
    if (typeof window !== 'undefined') {
      if ('scrollRestoration' in history) {
        history.scrollRestoration = 'manual';
      }
      window.scrollTo(0, 0);
    }

    afterNextRender(() => {
      window.scrollTo(0, 0);
      this.initScrollReveal();
    });
  }

  private initScrollReveal() {
    if (typeof window === 'undefined' || !('IntersectionObserver' in window)) return;

    // Stagger containers
    const staggerGroups = [
      { container: '.stats-row', item: '.stat-card' },
      { container: '.about-highlights-grid', item: '.highlight-chip' },
      { container: '.steps-row', item: '.step-card' },
      { container: '.branches-grid', item: '.branch-card' },
      { container: '.partners-grid', item: '.partner-card' },
      { container: '.footer-grid', item: '.footer-column' },
    ];

    staggerGroups.forEach((group) => {
      const parent = document.querySelector(group.container);
      if (parent) {
        const items = parent.querySelectorAll(group.item);
        items.forEach((item, index) => {
          item.classList.add('scroll-reveal');
          item.classList.add(`stagger-${Math.min(index + 1, 6)}`);
        });
      }
    });

    // Main section blocks to smoothly reveal
    const sectionTargets = [
      'app-stats-banner',
      'app-about .about-content',
      'app-about .about-visual-col',
      'app-features .reliable-title-col',
      'app-branches .branches-header',
      'app-partners .partners-header',
      'app-cta-banner .cta-banner-wrapper',
      '.scroll-reveal',
      '.reveal-on-scroll',
    ];

    const elements = document.querySelectorAll(sectionTargets.join(', '));

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add('is-revealed');
          }
        });
      },
      {
        threshold: 0.08,
        rootMargin: '0px 0px -30px 0px',
      }
    );

    elements.forEach((el) => {
      if (!el.classList.contains('scroll-reveal')) {
        el.classList.add('scroll-reveal');
      }
      observer.observe(el);
    });

    // Also observe all elements marked with scroll-reveal
    document.querySelectorAll('.scroll-reveal').forEach((el) => {
      observer.observe(el);
    });
  }
}

