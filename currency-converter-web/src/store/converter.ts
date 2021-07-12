import { addHoursToCurrent } from "@/utils/dateUtils";
import { rateKey } from "@/utils/rateUtils";
import axios, { CancelTokenSource } from "axios";
import { ActionTree, Module, MutationTree } from "vuex";
import { RootState } from ".";

export interface ConverterState {
  rates: { [key: string]: { rate: number; updateTime: Date } };
  ratesAreLoading: boolean;
  sourceCurrencyCode: string | undefined;
  targetCurrencyCode: string | undefined;
  cancelTokenSource: CancelTokenSource | undefined;
}

export enum ConverterMutationTypes {
  SET_RATE = "SET_RATE",
  SET_RATES_LOADING = "SET_RATES_LOADING",
  SET_SOURCE = "SET_SOURCE",
  SET_TARGET = "SET_TARGET",
  SET_CANCEL_TOKEN_SOURCE = "SET_CANCEL_TOKEN_SOURCE",
}

export enum ConverterActionTypes {
  REQUEST_RATE = "REQUEST_RATE",
}

export const mutations: MutationTree<ConverterState> = {
  [ConverterMutationTypes.SET_RATE](
    state,
    payload: { currencyPair: string; rate: number; updateTime: Date }
  ) {
    const { rate, updateTime, currencyPair } = payload;
    state.rates = {
      ...state.rates,
      [currencyPair]: { rate, updateTime },
    };
  },
  [ConverterMutationTypes.SET_RATES_LOADING](state, isLoading: boolean) {
    state.ratesAreLoading = isLoading;
  },
  [ConverterMutationTypes.SET_SOURCE](state, sourceCurrencyCode: string) {
    state.sourceCurrencyCode = sourceCurrencyCode;
  },
  [ConverterMutationTypes.SET_TARGET](state, targetCurrencyCode: string) {
    state.targetCurrencyCode = targetCurrencyCode;
  },
  [ConverterMutationTypes.SET_CANCEL_TOKEN_SOURCE](
    state,
    cancelTokenSource: CancelTokenSource | undefined
  ) {
    state.cancelTokenSource = cancelTokenSource;
  },
};

export const actions: ActionTree<ConverterState, RootState> = {
  async [ConverterActionTypes.REQUEST_RATE](
    { commit, state },
    payload: { sourceCurrencyCode: string; targetCurrencyCode: string }
  ) {
    const { sourceCurrencyCode, targetCurrencyCode } = payload;

    const currencyPair = rateKey(sourceCurrencyCode, targetCurrencyCode);
    const cachedRate = state.rates[currencyPair];
    if (cachedRate && cachedRate.updateTime > addHoursToCurrent(-1)) {
      return;
    }
    if (state.cancelTokenSource && state.cancelTokenSource.cancel) {
      state.cancelTokenSource.cancel();
    }
    commit(ConverterMutationTypes.SET_RATES_LOADING, true);
    const cancelTokenSource = axios.CancelToken.source();
    commit(ConverterMutationTypes.SET_CANCEL_TOKEN_SOURCE, cancelTokenSource);
    const uri = `${process.env.VUE_APP_CURRENCY_CONVERTER_API_URI}/ConversionRates/${sourceCurrencyCode}/${targetCurrencyCode}`;
    try {
      const response = await axios.get(uri, {
        cancelToken: cancelTokenSource.token,
      });
      const rate = response.data;
      commit(ConverterMutationTypes.SET_RATE, {
        currencyPair,
        rate,
        updateTime: new Date(),
      });
      commit(ConverterMutationTypes.SET_RATES_LOADING, false);
      commit(ConverterMutationTypes.SET_CANCEL_TOKEN_SOURCE, undefined);
    } catch (err) {
      if (axios.isCancel(err)) {
        console.log(`Cancelled: ${uri}`);
        // if request is cancelled it means that there is another one. No need to hide the spinner in this case
      } else {
        console.error(`Cannot get ${uri}: ${err}`);
        alert(err);
        commit(ConverterMutationTypes.SET_RATES_LOADING, false);
      }
    }
  },
};

const converter: Module<ConverterState, RootState> = {
  namespaced: true,
  state: {
    rates: {},
    ratesAreLoading: false,
    sourceCurrencyCode: undefined,
    targetCurrencyCode: undefined,
    cancelTokenSource: undefined,
  },
  mutations: mutations,
  actions: actions,
};

export default converter;
