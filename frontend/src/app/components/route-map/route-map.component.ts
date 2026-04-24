import { Component, Input, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import * as L from 'leaflet';

@Component({
  selector: 'app-route-map',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './route-map.component.html',
  styleUrls: ['./route-map.component.css']
})
export class RouteMapComponent implements OnInit, AfterViewInit, OnDestroy {
  @Input() source!: string;
  @Input() destination!: string;
  @Input() height: string = '400px';
  
  private map: L.Map | null = null;
  private markers: L.Marker[] = [];
  private routeLine: L.Polyline | null = null;
  public mapId: string = '';

  // Location coordinates for major Indian cities
  private locationCoordinates: { [key: string]: [number, number] } = {
    'Chennai': [13.0827, 80.2707],
    'Bangalore': [12.9716, 77.5946],
    'Mumbai': [19.0760, 72.8777],
    'Pune': [18.5204, 73.8567],
    'Hyderabad': [17.3850, 78.4867],
    'Delhi': [28.6139, 77.2090],
    'Kolkata': [22.5726, 88.3639],
    'Ahmedabad': [23.0225, 72.5714],
    'Jaipur': [26.9124, 75.7873],
    'Lucknow': [26.8467, 80.9462],
    'Coimbatore': [11.0168, 76.9558],
    'Madurai': [9.9252, 78.1198],
    'Trichy': [10.7905, 78.7047],
    'Salem': [11.6643, 78.1460],
    'Tirupur': [11.1085, 77.3398],
    'Erode': [11.3410, 77.7332],
    'Vellore': [12.9165, 79.1325],
    'Tirunelveli': [8.7139, 77.7567],
    'Thanjavur': [10.7870, 79.1378]
  };

  ngOnInit(): void {
    // Generate unique map ID
    this.mapId = 'routeMap_' + Math.random().toString(36).substr(2, 9);
    console.log('RouteMapComponent initialized with ID:', this.mapId);
  }

  ngAfterViewInit(): void {
    this.initializeMap();
  }

  ngOnDestroy(): void {
    this.destroyMap();
  }

  private initializeMap(): void {
    console.log('Initializing map with ID:', this.mapId, 'for route:', this.source, '→', this.destination);
    
    if (!this.source || !this.destination) {
      console.log('Missing source or destination, skipping map initialization');
      return;
    }

    // Initialize the map centered on India
    this.map = L.map(this.mapId).setView([20.5937, 78.9629], 5);
    console.log('Map created successfully');

    // Add OpenStreetMap tiles
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors'
    }).addTo(this.map);

    this.updateRoute();
  }

  private updateRoute(): void {
    if (!this.map || !this.source || !this.destination) {
      return;
    }

    // Clear existing markers and route
    this.clearMap();

    const sourceCoords = this.locationCoordinates[this.source];
    const destCoords = this.locationCoordinates[this.destination];

    if (!sourceCoords || !destCoords) {
      console.warn('Coordinates not found for source or destination');
      return;
    }

    // Create custom icons
    const sourceIcon = L.divIcon({
      html: '<div style="background-color: #4CAF50; color: white; border-radius: 50%; width: 30px; height: 30px; display: flex; align-items: center; justify-content: center; font-weight: bold; border: 2px solid white; box-shadow: 0 2px 4px rgba(0,0,0,0.3);">A</div>',
      iconSize: [30, 30],
      className: 'custom-div-icon'
    });

    const destIcon = L.divIcon({
      html: '<div style="background-color: #F44336; color: white; border-radius: 50%; width: 30px; height: 30px; display: flex; align-items: center; justify-content: center; font-weight: bold; border: 2px solid white; box-shadow: 0 2px 4px rgba(0,0,0,0.3);">B</div>',
      iconSize: [30, 30],
      className: 'custom-div-icon'
    });

    // Add markers
    const sourceMarker = L.marker(sourceCoords, { icon: sourceIcon })
      .addTo(this.map!)
      .bindPopup(`<strong>Source:</strong><br>${this.source}`);

    const destMarker = L.marker(destCoords, { icon: destIcon })
      .addTo(this.map!)
      .bindPopup(`<strong>Destination:</strong><br>${this.destination}`);

    this.markers.push(sourceMarker, destMarker);

    // Draw route line
    const routeCoords = [sourceCoords, destCoords];
    this.routeLine = L.polyline(routeCoords, {
      color: '#2196F3',
      weight: 4,
      opacity: 0.8,
      dashArray: '10, 10'
    }).addTo(this.map!);

    // Fit map to show the entire route
    const bounds = L.latLngBounds(routeCoords);
    this.map!.fitBounds(bounds, { padding: [50, 50] });

    // Add distance popup
    const distance = this.calculateDistance(sourceCoords, destCoords);
    const midPoint = [
      (sourceCoords[0] + destCoords[0]) / 2,
      (sourceCoords[1] + destCoords[1]) / 2
    ] as [number, number];

    L.popup()
      .setLatLng(midPoint)
      .setContent(`<strong>Route Distance:</strong><br>~${distance} km`)
      .openOn(this.map!);
  }

  private calculateDistance(coord1: [number, number], coord2: [number, number]): number {
    const R = 6371; // Earth's radius in kilometers
    const dLat = this.toRad(coord2[0] - coord1[0]);
    const dLon = this.toRad(coord2[1] - coord1[1]);
    const a = 
      Math.sin(dLat/2) * Math.sin(dLat/2) +
      Math.cos(this.toRad(coord1[0])) * Math.cos(this.toRad(coord2[0])) * 
      Math.sin(dLon/2) * Math.sin(dLon/2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
    return Math.round(R * c);
  }

  private toRad(value: number): number {
    return value * Math.PI / 180;
  }

  private clearMap(): void {
    // Remove markers
    this.markers.forEach(marker => {
      this.map?.removeLayer(marker);
    });
    this.markers = [];

    // Remove route line
    if (this.routeLine) {
      this.map?.removeLayer(this.routeLine);
      this.routeLine = null;
    }

    // Close all popups
    this.map?.closePopup();
  }

  private destroyMap(): void {
    if (this.map) {
      this.map.remove();
      this.map = null;
    }
  }
}
