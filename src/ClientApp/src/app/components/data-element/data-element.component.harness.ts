import { BaseHarnessFilters, ComponentHarness, HarnessPredicate } from '@angular/cdk/testing';

interface DataElementComponentHarnessFilters extends BaseHarnessFilters {
  /** Filters based on the id of the item. */
  id: string;
}

export class DataElementComponentHarness extends ComponentHarness {
  public static hostSelector = 'app-data-element';

  public static with(options: DataElementComponentHarnessFilters): HarnessPredicate<DataElementComponentHarness> {
    return new HarnessPredicate(DataElementComponentHarness, options)
      .addOption('id', options.id,
        (harness, id) => HarnessPredicate.stringMatches(harness.getId(), id));
  }

  public async getId(): Promise<string> {
    const elm = await this.host();
    const id = elm.getProperty('id');

    return id as unknown as string;
  }

  public async getLabel(): Promise<string> {
    const elm = await this.locatorFor('label')();
    const text = await elm.text();

    return text;
  }

  public async getValue(): Promise<string> {
    const elm = await this.locatorFor('div')();
    const text = await elm.text();

    return text;
  }
}
