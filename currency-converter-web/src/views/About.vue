

<template>
  <div class="about">
    <h1>Currency converter developed as a part of Octet interview process</h1>
    <h2>Author: {{ author }}</h2>
  </div>
</template>

<script lang="ts">
import { Vue } from "vue-class-component";
import { State } from "vuex-class";

export default class About extends Vue {
  @State("author") author!: string;
}
</script>