import {
  mutations,
  ConverterMutationTypes,
  ConverterState,
} from "@/store/converter";
import axios from "axios";

const currency = "NZD";
const date = new Date();
let state: ConverterState;

beforeEach(() => {
  state = {
    rates: {},
    ratesAreLoading: false,
    sourceCurrencyCode: undefined,
    targetCurrencyCode: undefined,
    cancelTokenSource: undefined,
  };
});

describe("ConverterStore.Mutations", () => {
  it("SET_RATE adds initial rate", () => {
    // Arrange, Act
    mutations[ConverterMutationTypes.SET_RATE](state, {
      currencyPair: "AUD_USD",
      rate: 0.1,
      updateTime: date,
    });

    // Assert
    const expected = { AUD_USD: { rate: 0.1, updateTime: date } };
    expect(state.rates).toMatchObject(expected);
  });

  it("SET_RATE adds subsequent rate", () => {
    // Arrange
    state.rates = { AUD_USD: { rate: 0.1, updateTime: date } };

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

  it("SET_RATES_LOADING sets ratesAreLoading", () => {
    // Arrange, Act
    mutations[ConverterMutationTypes.SET_RATES_LOADING](state, true);

    // Assert
    expect(state.ratesAreLoading).toBe(true);
  });

  it("SET_SOURCE sets sourceCurrencyCode", () => {
    // Arrange, Act
    mutations[ConverterMutationTypes.SET_SOURCE](state, currency);

    // Assert
    expect(state.sourceCurrencyCode).toBe(currency);
  });

  it("SET_TARGET sets targetCurrencyCode", () => {
    // Arrange, Act
    mutations[ConverterMutationTypes.SET_TARGET](state, currency);

    // Assert
    expect(state.targetCurrencyCode).toBe(currency);
  });

  it("SET_CANCEL_TOKEN sets cancelToken", () => {
    // Arrange, Act
    const cancelTokenSource = axios.CancelToken.source();
    mutations[ConverterMutationTypes.SET_CANCEL_TOKEN_SOURCE](
      state,
      cancelTokenSource
    );

    // Assert
    expect(state.cancelTokenSource).toBe(cancelTokenSource);
  });
});
