import Vue from 'vue'
import Router from 'vue-router'

import Home from './views/Home.vue'
import Dashboard from './views/Dashboard.vue'
import Highcharts from './views/Highcharts.vue'
import Settings from './views/Settings.vue'

import ProjectCoordinatesystem from './views/projects/CoordinateSystem.vue'
import ProjectTagFileUpload from './views/projects/Tagfiles.vue'

Vue.use(Router)

const DEFAULT_TITLE = "TRex Support Tool";

const router = new Router({
  mode: 'history',
  base: process.env.BASE_URL,
  routes: [{
    path: '/',
    alias: '/home',
    component: Home,
    children: [{
      path: "/project/tagfiles",
      component : ProjectTagFileUpload,
      meta : { title: "Tag File Upload "}
    }, {
      path: '/project/coordsystem',
      component: ProjectCoordinatesystem
    }, {
      path: '/settings',
      component: Settings
    }] 
  }]
});

router.afterEach((to, from) => {
    document.title =  '(' + config.VUE_APP_ENV + ') '  + (to.meta.title || DEFAULT_TITLE);
});

export default router;