import { enableProdMode, importProvidersFrom, provideExperimentalZonelessChangeDetection } from '@angular/core';
import { environment } from './environments/environment';
import { AppComponent } from './app/app.component';
import { withInterceptorsFromDi, provideHttpClient } from '@angular/common/http';
import { BrowserModule, bootstrapApplication } from '@angular/platform-browser';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
  providers: [
    importProvidersFrom(BrowserModule),
    provideHttpClient(withInterceptorsFromDi()),
    provideAnimationsAsync(),
    provideExperimentalZonelessChangeDetection(),
    provideCharts(withDefaultRegisterables()),
  ],
})
  .catch(err => console.error(err));
