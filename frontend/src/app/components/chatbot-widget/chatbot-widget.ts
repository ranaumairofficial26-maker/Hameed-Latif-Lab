import { Component, ElementRef, ViewChild, inject, ChangeDetectorRef, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

interface ChatMessage {
  id: string;
  sender: 'user' | 'bot';
  text: string;
  time: string;
  isGrounded?: boolean;
}

@Component({
  selector: 'app-chatbot-widget',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chatbot-widget.html',
  styleUrl: './chatbot-widget.css',
})
export class ChatbotWidget implements OnInit {
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef);

  @ViewChild('messagesContainer') private messagesContainer?: ElementRef<HTMLDivElement>;

  isOpen = signal(false);
  isBadgeDismissed = signal(false);
  inputText = '';
  isLoading = signal(false);
  sessionId = 'sess_' + Math.random().toString(36).substring(2, 10);

  ngOnInit() {
    // Warm up backend on hosting services (e.g. Render spin-down wake up)
    try {
      const healthUrl = environment.apiUrl.replace(/\/api\/?$/, '') + '/health';
      this.http.get(healthUrl, { responseType: 'text' }).subscribe({
        next: () => {},
        error: () => {},
      });
    } catch (_) {}
  }

  messages = signal<ChatMessage[]>([
    {
      id: 'msg-welcome',
      sender: 'bot',
      text: 'Assalam-o-Alaikum! Welcome to Hammad Latif Lab Assistant. How can I help you today? You can ask about lab timings, test preparation & fasting, home sampling, or online reports.',
      time: this.getCurrentTime(),
      isGrounded: true,
    },
  ]);

  toggleChat() {
    const nextState = !this.isOpen();
    this.isOpen.set(nextState);
    if (nextState) {
      this.isBadgeDismissed.set(true);
      setTimeout(() => this.scrollToBottom(), 100);
    }
  }

  dismissBadge(event: MouseEvent) {
    event.stopPropagation();
    this.isBadgeDismissed.set(true);
  }

  sendMessage() {
    const text = this.inputText.trim();
    if (!text || this.isLoading()) return;

    // 1. Append User Message
    const userMsg: ChatMessage = {
      id: 'usr-' + Date.now(),
      sender: 'user',
      text: text,
      time: this.getCurrentTime(),
    };
    this.messages.update((msgs) => [...msgs, userMsg]);
    this.inputText = '';
    this.isLoading.set(true);
    this.cdr.markForCheck();
    this.scrollToBottom();

    // 2. Call .NET Backend API
    const apiUrl = `${environment.apiUrl}/Chat/message`;
    const payload = {
      message: text,
      sessionId: this.sessionId,
    };

    this.http.post<any>(apiUrl, payload).subscribe({
      next: (res) => {
        if (res?.sessionId) {
          this.sessionId = res.sessionId;
        }
        const botMsg: ChatMessage = {
          id: 'bot-' + Date.now(),
          sender: 'bot',
          text: res?.answer || 'Thank you for contacting Hammad Latif Lab.',
          time: this.getCurrentTime(),
          isGrounded: res?.isGrounded ?? true,
        };
        this.messages.update((msgs) => [...msgs, botMsg]);
        this.isLoading.set(false);
        this.cdr.markForCheck();
        this.cdr.detectChanges();
        this.scrollToBottom();
      },
      error: (err) => {
        console.error('Chatbot API error:', err);
        const fallbackMsg: ChatMessage = {
          id: 'bot-err-' + Date.now(),
          sender: 'bot',
          text: 'I am temporarily unable to reach the laboratory server. Please call our 24/7 Helpline at +92-42-111-455-522 for immediate assistance.',
          time: this.getCurrentTime(),
          isGrounded: false,
        };
        this.messages.update((msgs) => [...msgs, fallbackMsg]);
        this.isLoading.set(false);
        this.cdr.markForCheck();
        this.cdr.detectChanges();
        this.scrollToBottom();
      },
    });
  }

  private getCurrentTime(): string {
    const now = new Date();
    return now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  private scrollToBottom() {
    setTimeout(() => {
      if (this.messagesContainer?.nativeElement) {
        this.messagesContainer.nativeElement.scrollTop = this.messagesContainer.nativeElement.scrollHeight;
      }
    }, 50);
  }
}
