import { shallowMount } from "@vue/test-utils";
import CurrencyConverter from "@/components/CurrencyConverter.vue";
import { RootState } from "@/store/index";
import {
  ConverterActionTypes,
  ConverterMutationTypes,
  ConverterState,
} from "@/store/converter";
import { createStore, Module, Store } from "vuex";

const createMockStore = (
  mockRequestRate: () => void = jest.fn()
): Store<RootState> => {
  const converter: Module<ConverterState, RootState> = {
    namespaced: true,
    state: {
      rates: {},
      ratesAreLoading: false,
      sourceCurrencyCode: undefined,
      targetCurrencyCode: undefined,
    },
    mutations: {
      [ConverterMutationTypes.SET_RATE]: jest.fn(),
      [ConverterMutationTypes.SET_RATES_LOADING]: jest.fn(),
      [ConverterMutationTypes.SET_SOURCE](state, sourceCurrencyCode: string) {
        state.sourceCurrencyCode = sourceCurrencyCode;
      },
      [ConverterMutationTypes.SET_TARGET](state, targetCurrencyCode: string) {
        state.targetCurrencyCode = targetCurrencyCode;
      },
    },
    actions: {
      [ConverterActionTypes.REQUEST_RATE]: mockRequestRate,
    },
  };
  const store = createStore<RootState>({
    modules: {
      converter: converter,
    },
  });
  return store;
};

describe("CurrencyConverter.vue", () => {
  it("USD is default source currency", () => {
    // Arrange, Act
    const wrapper = shallowMount(CurrencyConverter, {
      global: {
        plugins: [createMockStore()],
      },
    });

    // Assert
    expect(
      wrapper.find<HTMLSelectElement>(".source-currency-code").text()
    ).toMatch("USD");
  });

  it("AUD is default target currency", () => {
    // Arrange, Act
    const wrapper = shallowMount(CurrencyConverter, {
      global: {
        plugins: [createMockStore()],
      },
    });

    // Assert
    expect(
      wrapper.find<HTMLSelectElement>(".target-currency-code").text()
    ).toMatch("AUD");
  });

  it("Generates reverse mappings", async () => {
    // Arrange
    const wrapper = shallowMount(CurrencyConverter, {
      global: {
        plugins: [createMockStore()],
      },
    });
    const sourceSelect = wrapper.find<HTMLSelectElement>(
      ".source-currency-code"
    );
    const sourceOptions = sourceSelect.findAll("option");

    // Act
    await (sourceOptions[sourceOptions.length - 1] as any).setSelected();

    // Assert
    const targetSelect = wrapper.find<HTMLSelectElement>(
      ".target-currency-code"
    );
    const targetOptions = targetSelect.findAll("option");
    expect(targetOptions.length).toBe(4);
  });

  it("Requests rate when created", async () => {
    // Arrange
    const mockRequestRate = jest.fn();
    const store = createMockStore(mockRequestRate);

    // Act
    shallowMount(CurrencyConverter, {
      global: {
        plugins: [store],
      },
    });

    // Assert
    expect(mockRequestRate).toHaveBeenCalledTimes(1);
  });

  it("Requests rate when source is changed", async () => {
    // Arrange
    const mockRequestRate = jest.fn();
    const store = createMockStore(mockRequestRate);
    const wrapper = shallowMount(CurrencyConverter, {
      global: {
        plugins: [store],
      },
    });
    mockRequestRate.mockReset();
    const sourceSelect = wrapper.find<HTMLSelectElement>(
      ".source-currency-code"
    );
    const sourceOptions = sourceSelect.findAll("option");

    // Act
    await (sourceOptions[sourceOptions.length - 1] as any).setSelected();

    // Assert
    expect(mockRequestRate).toHaveBeenCalledTimes(1);
  });

  it("Requests rate when target is changed", async () => {
    // Arrange
    const mockRequestRate = jest.fn();
    const store = createMockStore(mockRequestRate);
    const wrapper = shallowMount(CurrencyConverter, {
      global: {
        plugins: [store],
      },
    });

    // Several target options are available only for the last source option (AUD), thus we need to select it first
    const sourceSelect = wrapper.find<HTMLSelectElement>(
      ".source-currency-code"
    );
    const sourceOptions = sourceSelect.findAll<HTMLOptionElement>("option");
    await (sourceOptions[sourceOptions.length - 1] as any).setSelected();
    const targetSelect = wrapper.find<HTMLSelectElement>(
      ".target-currency-code"
    );
    const targetOptions = targetSelect.findAll("option");
    mockRequestRate.mockReset();

    // Act
    await (targetOptions[targetOptions.length - 1] as any).setSelected();

    // Assert
    expect(mockRequestRate).toHaveBeenCalledTimes(1);
  });

  it("Swap button swaps currencies", async () => {
    // Arrange
    const store = createMockStore();
    const wrapper = shallowMount(CurrencyConverter, {
      global: {
        plugins: [store],
      },
    });
    const sourceSelect = wrapper.find<HTMLSelectElement>(
      ".source-currency-code"
    );
    const targetSelect = wrapper.find<HTMLSelectElement>(
      ".target-currency-code"
    );
    const swapButton = wrapper.find(".swap");

    const initialSource = sourceSelect.element.value;
    const initialTarget = targetSelect.element.value;

    console.log(initialSource);

    // Act
    await swapButton.trigger("click");

    // Assert
    const finalSource = sourceSelect.element.value;
    const finalTarget = targetSelect.element.value;

    expect(finalSource).toBe(initialTarget);
    expect(finalTarget).toBe(initialSource);
  });
});
