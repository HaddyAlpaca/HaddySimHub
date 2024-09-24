export const createSpyObj = <T>(methodNames: (keyof T)[]): jest.Mocked<T> => {
  const obj: { [K in keyof T]?: jest.Mock } = {};
  methodNames.forEach((methodName) => {
    obj[methodName] = jest.fn();
  });
  return obj as jest.Mocked<T>;
}
