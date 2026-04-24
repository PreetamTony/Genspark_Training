import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-bus-details',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './bus-details.component.html',
  styleUrl: './bus-details.component.css'
})
export class BusDetailsComponent {
  @Input() bus: any;
  @Input() schedule: any;
  
  showAllBoardingPoints = false;
  showAllDroppingPoints = false;
  showAllReviews = false;
  activeTab: string = 'highlights';

  constructor() {}

  getDuration(departureTime: string, arrivalTime: string): string {
    if (!departureTime || !arrivalTime) return '--h --m';
    try {
      // Helper function to parse dates robustly
      const parseDate = (time: string): Date => {
        let date = new Date(time);
        if (isNaN(date.getTime())) {
          const cleanedTime = time.replace('Z', '');
          date = new Date(cleanedTime + 'Z');
        }
        return date;
      };

      const depart = parseDate(departureTime);
      const arrive = parseDate(arrivalTime);
      
      if (isNaN(depart.getTime()) || isNaN(arrive.getTime())) {
        console.warn('Invalid duration dates:', { departureTime, arrivalTime });
        return '--h --m';
      }
      
      const diffMs = arrive.getTime() - depart.getTime();
      const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
      const diffMinutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));
      return `${diffHours}h ${diffMinutes}m`;
    } catch (error) {
      console.error('Error calculating duration:', { departureTime, arrivalTime, error });
      return '--h --m';
    }
  }

  formatTime(time: string): string {
    if (!time || time === undefined || time === null) {
      return '--:--';
    }
    try {
      const date = new Date(time);
      if (isNaN(date.getTime())) {
        return '--:--';
      }
      
      return date.toLocaleTimeString('en-US', { 
        hour: '2-digit', 
        minute: '2-digit',
        hour12: false 
      });
    } catch (error) {
      return '--:--';
    }
  }

  formatDate(time: string): string {
    if (!time || time === undefined || time === null) {
      return 'Invalid Date';
    }
    try {
      const date = new Date(time);
      if (isNaN(date.getTime())) {
        return 'Invalid Date';
      }
      
      return date.toLocaleDateString('en-US', { 
        month: 'short', 
        day: 'numeric'
      });
    } catch (error) {
      return 'Invalid Date';
    }
  }

  getFeatureIcon(feature: string): string {
    const icons: { [key: string]: string } = {
      'hasWaterBottle': '💧',
      'hasBlankets': '🛏️',
      'hasChargingPoint': '🔌',
      'hasCCTV': '📹',
      'hasToilet': '🚽',
      'hasWiFi': '📶',
      'hasReadingLight': '💡',
      'hasEmergencyExit': '🚪',
      'hasGPS': '📍'
    };
    return icons[feature] || '✓';
  }

  getFeatureName(feature: string): string {
    const names: { [key: string]: string } = {
      'hasWaterBottle': 'Water Bottle',
      'hasBlankets': 'Blankets',
      'hasChargingPoint': 'Charging Point',
      'hasCCTV': 'CCTV',
      'hasToilet': 'Toilet',
      'hasWiFi': 'WiFi',
      'hasReadingLight': 'Reading Light',
      'hasEmergencyExit': 'Emergency Exit',
      'hasGPS': 'GPS Tracking'
    };
    return names[feature] || feature;
  }

  getActiveFeatures(): string[] {
    if (!this.bus?.features) return [];
    return Object.keys(this.bus.features).filter(key => this.bus.features[key] === true);
  }

  getRatingStars(rating: number): string[] {
    const stars = [];
    for (let i = 1; i <= 5; i++) {
      if (i <= rating) {
        stars.push('⭐');
      } else if (i - 0.5 <= rating) {
        stars.push('⭐');
      } else {
        stars.push('☆');
      }
    }
    return stars;
  }

  toggleBoardingPoints() {
    this.showAllBoardingPoints = !this.showAllBoardingPoints;
  }

  toggleDroppingPoints() {
    this.showAllDroppingPoints = !this.showAllDroppingPoints;
  }

  toggleReviews() {
    this.showAllReviews = !this.showAllReviews;
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
  }

  parseCancellationPolicy(policy: string): any[] {
    if (!policy) return [];
    const lines = policy.split(';');
    return lines.map(line => {
      const parts = line.split('-');
      return {
        timeWindow: parts[0]?.trim() || '',
        refund: parts[1]?.trim() || ''
      };
    }).filter(item => item.timeWindow && item.refund);
  }
}
