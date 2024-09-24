import { Component, computed, input } from '@angular/core';
import { DataType } from '@components/data-element/data-element.component';

@Component({
  template: '',
})
export abstract class DisplayComponent<T> {
  protected abstract checkDataType(data: unknown): boolean;
  protected abstract createDefaultData(): T;

  public readonly DataType = DataType;

  public dataSource = input.required<unknown>({});

  public data = computed(() => {
    const data = this.dataSource();

    return data ? data as T : this.createDefaultData();
  });
}
