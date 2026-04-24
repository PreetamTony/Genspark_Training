import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chatbot.component.html',
  styleUrls: ['./chatbot.component.css']
})
export class ChatbotComponent implements OnInit, OnDestroy {
  isOpen = false;
  messages: Array<{type: 'user' | 'bot', content: string, timestamp: Date}> = [];
  currentMessage = '';
  isTyping = false;
  
  private apiUrl = 'http://localhost:5047/api/chatbot';

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    // Add welcome message
    this.addBotMessage('Hello! I\'m Nexbot, your AI assistant for NexBus. I can help you with bus bookings, route information, schedules, and travel assistance. How can I help you today?');
  }

  ngOnDestroy(): void {
    // Cleanup if needed
  }

  toggleChat(): void {
    this.isOpen = !this.isOpen;
  }

  sendMessage(): void {
    if (!this.currentMessage.trim()) return;

    const userMessage = this.currentMessage.trim();
    this.addUserMessage(userMessage);
    this.currentMessage = '';
    this.isTyping = true;

    // Send message to backend
    this.sendMessageToBot(userMessage);
  }

  private sendMessageToBot(message: string): void {
    this.http.post<{message: string}>(`${this.apiUrl}/chat`, { message })
      .subscribe({
        next: (response) => {
          const botMessage = response.message || 'Sorry, I could not process your request.';
          this.addBotMessage(botMessage);
          this.isTyping = false;
          this.cdr.detectChanges(); // Force immediate UI update
        },
        error: (error) => {
          console.error('Error sending message:', error);
          this.addBotMessage('Sorry, I encountered an error. Please try again later.');
          this.isTyping = false;
          this.cdr.detectChanges(); // Force immediate UI update
        }
      });
  }

  private addUserMessage(message: string): void {
    this.messages.push({
      type: 'user',
      content: message,
      timestamp: new Date()
    });
    this.cdr.detectChanges(); // Force immediate UI update
    this.scrollToBottom();
  }

  private addBotMessage(message: string): void {
    this.messages.push({
      type: 'bot',
      content: message,
      timestamp: new Date()
    });
    this.cdr.detectChanges(); // Force immediate UI update
    this.scrollToBottom();
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      const chatContainer = document.querySelector('.chat-messages');
      if (chatContainer) {
        chatContainer.scrollTop = chatContainer.scrollHeight;
      }
    }, 100);
  }

  handleKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  clearChat(): void {
    this.messages = [];
    this.addBotMessage('Chat cleared. How can I help you today?');
  }

  getQuickSuggestions(): string[] {
    const suggestions = [
      'What buses are available from Chennai to Bangalore?',
      'How do I cancel my booking?',
      'What are the payment methods?',
      'What is the luggage allowance?',
      'How do I contact customer support?',
      'What are the bus types available?'
    ];
    
    // Return random suggestions
    return suggestions.sort(() => 0.5 - Math.random()).slice(0, 3);
  }

  useSuggestion(suggestion: string): void {
    this.currentMessage = suggestion;
    this.sendMessage();
  }

  formatTime(date: Date): string {
    return date.toLocaleTimeString('en-US', { 
      hour: '2-digit', 
      minute: '2-digit',
      hour12: true 
    });
  }
}
