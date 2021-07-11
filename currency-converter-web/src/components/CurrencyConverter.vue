<template>
  <div>
    <div>
      <select
        class="source-currency-code"
        :value="source"
        @change="onSourceChange($event)"
      >
        <option v-for="option in sources" :key="option" :value="option">
          {{ option }}
        </option>
      </select>
      <ArrowIcon />
      <select
        class="target-currency-code"
        :value="target"
        @change="onTargetChange($event)"
      >
        <option v-for="option in targets" :key="option" :value="option">
          {{ option }}
        </option>
      </select>
      <SwapButton class="swap" @click="swap" />
    </div>
    <h1 v-if="selectedRate">
      1 {{ source }} = {{ selectedRate }} {{ target }}
    </h1>
    <div v-else-if="ratesAreLoading" class="loader">Loading...</div>
  </div>
</template>

<script lang="ts">
import {
  ConverterActionTypes,
  ConverterMutationTypes,
} from "@/store/converter";
import { rateKey } from "@/utils/rateUtils";
import { Options, Vue } from "vue-class-component";
import { namespace } from "vuex-class";
import SwapButton from "@/components/SwapButton.vue";
import ArrowIcon from "@/components/ArrowIcon.vue";

const converter = namespace("converter");
@Options({
  components: {
    SwapButton,
    ArrowIcon,
  },
})
export default class CurrencyConverter extends Vue {
  @converter.Action(ConverterActionTypes.REQUEST_RATE)
  requestRate!: (payload: {
    sourceCurrencyCode: string;
    targetCurrencyCode: string;
  }) => void;

  @converter.Mutation(ConverterMutationTypes.SET_SOURCE) setSource!: (
    sourceCurrencyCode: string
  ) => void;

  @converter.Mutation(ConverterMutationTypes.SET_TARGET) setTarget!: (
    targetCurrencyCode: string
  ) => void;

  @converter.State("sourceCurrencyCode") source!: string;
  @converter.State("targetCurrencyCode") target!: string;

  @converter.State("rates") savedRates!: {
    [key: string]: { rate: number; updateTime: Date };
  };

  @converter.State("ratesAreLoading") ratesAreLoading!: boolean;

  private currencyMappings: { [key: string]: [string] } = {
    USD: ["AUD"],
    NZD: ["AUD"],
    JPY: ["AUD"],
    CNY: ["AUD"],
  };

  get selectedRate(): number | undefined {
    var key = rateKey(this.source, this.target);
    if (key in this.savedRates) {
      return this.savedRates[key].rate;
    }
    return undefined;
  }

  get sources(): string[] {
    return Object.keys(this.currencyMappings);
  }

  get targets(): string[] {
    return this.currencyMappings[this.source];
  }

  onSourceChange(event: any) {
    const source = event.target.value;
    this.setSource(source);
    var target = this.currencyMappings[source][0];
    this.setTarget(target);
    this.requestRate({
      sourceCurrencyCode: event.target.value,
      targetCurrencyCode: target,
    });
  }

  onTargetChange(event: any) {
    const target = event.target.value;
    this.setTarget(target);
    this.requestRate({
      sourceCurrencyCode: this.source,
      targetCurrencyCode: target,
    });
  }

  swap(): void {
    const source = this.source;
    const target = this.target;
    this.setTarget(source);
    this.setSource(target);
    this.requestRate({
      sourceCurrencyCode: target,
      targetCurrencyCode: source,
    });
  }

  created(): void {
    if (!this.source && this.sources.length) {
      this.setSource(this.sources[0]);
    }
    if (!this.target && this.targets.length) {
      this.setTarget(this.targets[0]);
    }
    this.getReverseMappings();
    this.requestRate({
      sourceCurrencyCode: this.source,
      targetCurrencyCode: this.target,
    });
  }

  private getReverseMappings() {
    for (const key of Object.keys(this.currencyMappings)) {
      const value = this.currencyMappings[key];
      for (const target of value) {
        if (target in this.currencyMappings) {
          this.currencyMappings[target].push(key);
        } else {
          this.currencyMappings[target] = [key];
        }
      }
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped src="@/assets/spinner.css"></style>
