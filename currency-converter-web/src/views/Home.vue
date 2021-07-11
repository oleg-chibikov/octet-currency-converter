<template>
  <div class="home">
    <CurrencyConverter />
  </div>
</template>

<script lang="ts">
import { Options, Vue } from "vue-class-component";
import CurrencyConverter from "@/components/CurrencyConverter.vue"; // @ is an alias to /src

@Options({
  components: {
    CurrencyConverter,
  },
})
export default class Home extends Vue {}
</script>
