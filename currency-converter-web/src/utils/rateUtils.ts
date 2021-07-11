export const rateKey = (
  sourceCurrencyCode: string,
  targetCurrencyCode: string
): string => sourceCurrencyCode + "_" + targetCurrencyCode;
