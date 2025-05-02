import { ComponentFixture, TestBed } from "@angular/core/testing";
import { RelativePageComponent } from "./relative-page.component";
import { TestbedHarnessEnvironment } from "@angular/cdk/testing/testbed";
import { RelativePageComponentHarness, TimingEntry } from "./relative-page.component.harness";
import { Component, provideExperimentalZonelessChangeDetection } from "@angular/core";

describe('RelativePageComponent tests', () => {
    let fixture: ComponentFixture<RelativePageTestHostComponent>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            providers: [
              provideExperimentalZonelessChangeDetection(),
            ],
          }).compileComponents();
      
        fixture = TestBed.createComponent(RelativePageTestHostComponent);
    });
    
    it('Check order when player is in the middle', async () => {
        const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, RelativePageComponentHarness);
        const entries = [
            { isPlayer: false, isSafetyCar: false, isInPits: false, position: '1', carNumber: '1', driverName: 'Driver 1', license: 'A', iRating: '1000', timeToPlayer: '0' },
            { isPlayer: true, isSafetyCar: false, isInPits: false, position: '2', carNumber: '2', driverName: 'Driver 2', license: 'B', iRating: '2000', timeToPlayer: '-5' },
            { isPlayer: false, isSafetyCar: false, isInPits: false, position: '3', carNumber: '3', driverName: 'Driver 3', license: 'C', iRating: '3000', timeToPlayer: '-10' },
            { isPlayer: false, isSafetyCar: false, isInPits: false, position: '4', carNumber: '4', driverName: 'Driver 4', license: 'D', iRating: '4000', timeToPlayer: '-15' },
            { isPlayer: false, isSafetyCar: false, isInPits: false, position: '5', carNumber: '5', driverName: 'Driver 5', license: 'E', iRating: '5000', timeToPlayer: '-20' },
            { isPlayer: false, isSafetyCar: true, isInPits: false, position: '', carNumber:'SC', driverName:'Safety Car', license:'SC', iRating:'SC', timeToPlayer:'-25' },
        ];

        
    });
});

@Component({
    selector: 'app-relative-page-test-host',
    template: `<app-relative-page [timingEntries]="timingEntries" />`,
    imports: [RelativePageComponent],
})
class RelativePageTestHostComponent {
    public timingEntries: TimingEntry[] = [];
}