const config = (() => {
    return {
      // NOTE This file will be overwritten via deploy scripts
      // Any changes to env variables for dev / alpha / prod need to make in k8s conifg
      // Only local settings should be stored here
//      "VUE_APP_TREX_MUTABLE_GATEWAY_URL": "http://trex.mutable.dev.eks.ccss.cloud",
      "VUE_APP_TREX_MUTABLE_GATEWAY_URL": "http://localhost:55650",
      "VUE_APP_ENV" : "local"
    };
  })();