import {
  ConverterState,
  actions,
  ConverterActionTypes,
  ConverterMutationTypes,
} from "@/store/converter";
import { addMinutesToCurrent } from "@/utils/dateUtils";
import { rateKey } from "@/utils/rateUtils";
let state: ConverterState;
import axios from "axios";
import MockAdapter from "axios-mock-adapter";

const sourceCurrencyCode = "AUD";
const targetCurrencyCode = "NZD";
const commit = jest.fn();
const mockAxios = new MockAdapter(axios);
const uriToMock = `${process.env.VUE_APP_CURRENCY_CONVERTER_API_URI}/ConversionRates/${sourceCurrencyCode}/${targetCurrencyCode}`;
const returnedRate = 0.11;
mockAxios.onGet(uriToMock).reply(200, returnedRate);
const requestRateAction = actions[ConverterActionTypes.REQUEST_RATE] as any;

beforeEach(() => {
  state = {
    rates: {},
    ratesAreLoading: false,
    sourceCurrencyCode: undefined,
    targetCurrencyCode: undefined,
    cancelTokenSource: undefined,
  };
  commit.mockReset();
});

describe("Actions", () => {
  it("REQUEST_RATE sets and resets isLoading", async () => {
    // Arrange, Act
    await requestRateAction(
      { commit, state },
      { sourceCurrencyCode, targetCurrencyCode }
    );

    // Assert
    expect(commit).toHaveBeenCalledWith(
      ConverterMutationTypes.SET_RATES_LOADING,
      true
    );
    expect(commit).toHaveBeenCalledWith(
      ConverterMutationTypes.SET_RATES_LOADING,
      false
    );
  });

  it("REQUEST_RATE sets rate", async () => {
    // Arrange
    const currencyPair = rateKey(sourceCurrencyCode, targetCurrencyCode);

    // Act
    await requestRateAction(
      { commit, state },
      { sourceCurrencyCode, targetCurrencyCode }
    );

    // Assert
    const setRatePayload = commit.mock.calls.filter(
      (call) => call[0] === ConverterMutationTypes.SET_RATE
    )[0][1];
    expect(setRatePayload.currencyPair).toBe(currencyPair);
    expect(setRatePayload.rate).toBe(returnedRate);
  });

  it("REQUEST_RATE doesn't set rate when cached rate is not expired", async () => {
    // Arrange
    const currencyPair = rateKey(sourceCurrencyCode, targetCurrencyCode);
    const cachedRate = 0.77;
    state.rates = {
      [currencyPair]: {
        rate: cachedRate,
        updateTime: addMinutesToCurrent(-59),
      },
    };

    // Act
    await requestRateAction(
      { commit, state },
      { sourceCurrencyCode, targetCurrencyCode }
    );

    // Assert
    expect(commit).not.toHaveBeenCalled();
  });

  it("REQUEST_RATE sets rate when cached rate is expired", async () => {
    // Arrange
    const currencyPair = rateKey(sourceCurrencyCode, targetCurrencyCode);
    const cachedRate = 0.77;
    state.rates = {
      [currencyPair]: {
        rate: cachedRate,
        updateTime: addMinutesToCurrent(-61),
      },
    };

    // Act
    await requestRateAction(
      { commit, state },
      { sourceCurrencyCode, targetCurrencyCode }
    );

    // Assert
    const setRatePayload = commit.mock.calls.filter(
      (call) => call[0] === ConverterMutationTypes.SET_RATE
    )[0][1];
    expect(setRatePayload.currencyPair).toBe(currencyPair);
    expect(setRatePayload.rate).toBe(returnedRate);
  });

  // TODO: tests for cancellation: make mock execute for unlimited time for a first call and make a second call while the first is being executed. Verify that the first call is cancelled
});
