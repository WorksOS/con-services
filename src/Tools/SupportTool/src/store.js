import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    appSettings: {
      dark: false,
      drawers: ['Default (no property)', 'Permanent', 'Temporary'],
      primaryDrawer: {
        model: null,
        type: 'permanent',
        clipped: true,
        floating: false,
        mini: false
      },
      footer: {
        inset: false
      }
    },
    highChartsLoaded: false,
    highChartsData: null
  },
  mutations: {
    setHighChartsLoaded(state, {
      payload
    }) {
      state.highChartsLoaded = payload
    },
    setHighChartsData(state, {
      payload
    }) {
      state.highChartsData = payload
    }
  },
  actions: {
    chartLoaded: ({
      commit
    }, payload) => commit('setHighChartsLoaded', {
      payload
    }),
    chartData: ({
      commit
    }, payload) => commit('setHighChartsData', {
      payload
    })
  }
})