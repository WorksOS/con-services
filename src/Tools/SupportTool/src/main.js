import '@mdi/font/css/materialdesignicons.css' // Ensure you are using css-loader

import Vue from 'vue'
import Vuetify from 'vuetify'
import 'vuetify/dist/vuetify.min.css'

import App from './App.vue'
import router from './router'
import store from './store'
import helpers from './helpers'

import FileSelect from "./components/FileSelect.vue"

Vue.config.productionTip = false

const opts = {
  theme: {
    // dark: false,
    themes: {
      light: {
        // primary: '...',
      },
      dark: {
        // primary: '...',
      }
    },
    icons: {
      iconfont: 'fa4',
      // values: { ... }
    }
  }
}

Vue.use(Vuetify)

Vue.component('file-select', FileSelect);
Vue.mixin(helpers);

console.log("Running in", config.VUE_APP_ENV);

new Vue({
  router,
  store,
  vuetify: new Vuetify(opts),
  render: h => h(App)
}).$mount('#app')