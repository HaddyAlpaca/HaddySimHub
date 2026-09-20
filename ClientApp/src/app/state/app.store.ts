import { DisplayType, type DisplayUpdate } from '../sse.service';

type Listener = () => void;

export class AppStore {
  private _displayUpdate: DisplayUpdate = { type: DisplayType.None, data: undefined };
  private readonly _listeners = new Set<Listener>();

  public subscribe(listener: Listener): () => void {
    this._listeners.add(listener);
    return () => this._listeners.delete(listener);
  }

  public updateDisplay(displayUpdate: DisplayUpdate): void {
    this._displayUpdate = displayUpdate;
    this._listeners.forEach((listener) => listener());
  }

  public get displayType(): DisplayType {
    return this._displayUpdate.type ?? DisplayType.None;
  }

  public get data(): DisplayUpdate['data'] {
    return this._displayUpdate.data;
  }
}
