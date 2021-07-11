import {
  mutations,
  ConverterMutationTypes,
  ConverterState,
} from "@/store/converter";

describe("Mutations", () => {
  it("SetRate adds initial rate", () => {
    // Arrange
    const date = new Date();
    const state: ConverterState = {
      rates: {},
      ratesAreLoading: false,
      sourceCurrencyCode: undefined,
      targetCurrencyCode: undefined,
    };

    // Act
    mutations[ConverterMutationTypes.SET_RATE](state, {
      currencyPair: "AUD_USD",
      rate: 0.1,
      updateTime: date,
    });

    // Assert
    const expected = { AUD_USD: { rate: 0.1, updateTime: date } };
    expect(state.rates).toMatchObject(expected);
  });

  it("SetRate adds subsequent rate", () => {
    // Arrange
    const date = new Date();
    const state: ConverterState = {
      rates: { AUD_USD: { rate: 0.1, updateTime: date } },
      ratesAreLoading: false,
      sourceCurrencyCode: undefined,
      targetCurrencyCode: undefined,
    };

    // Act
    mutations[ConverterMutationTypes.SET_RATE](state, {
      currencyPair: "AUD_JPY",
      rate: 0.2,
      updateTime: date,
    });

    // Assert
    const expected = {
      AUD_USD: { rate: 0.1, updateTime: date },
      AUD_JPY: { rate: 0.2, updateTime: date },
    };
    expect(state.rates).toMatchObject(expected);
  });
});
