import { createStore } from "vuex";
import VuexPersistence from "vuex-persist";
import converter from "./converter";

export interface RootState {
  author: string;
}

const vuexLocal = new VuexPersistence<RootState>({
  storage: window.localStorage,
});

export default createStore<RootState>({
  state: {
    author: "Oleg Chibikov",
  },
  modules: {
    converter: converter,
  },
  plugins: [vuexLocal.plugin],
});
