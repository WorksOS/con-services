<template>
  <!--
    Everything is wrapped in a label, which acts as a clickable wrapper around a form element.
    In this case, the file input.
  -->
   <v-card outlined>
    <v-list-item two-line>
      <v-list-item-content>
        <div class="overline mb-4">{{ prompt }}</div>
        <v-list-item-title class="headline mb-1" v-if="value">{{value.name}}</v-list-item-title>
        <v-list-item-title class="headline mb-1" v-else>No file selected</v-list-item-title>
      </v-list-item-content>
    </v-list-item>

    <v-card-actions>
      <v-btn class="select-button ma-2" color="primary" @click="$refs.inputUpload.click()">Select File</v-btn>
      <v-btn class="cancel-button ma-2" color="warning" v-show="value" @click="clearSelected">Clear Selected File</v-btn>
    </v-card-actions>
    <!-- referenced by button-->
    <input v-show="false" type="file" ref="inputUpload" @change="handleFileChange"/>
  </v-card>
</template>

<style>

.select-button {
    
}

.cancel-button {
    padding-right: 50px;
}

.v-btn {
  min-width: 0;
}

</style>

<script>
export default {
  props: {
    // Using value here allows us to be v-model compatible.
    value: File,
    prompt: {
        type: String,
        default : "Select a file"
    }
  },

  methods: {
    handleFileChange(e) {
      // Whenever the file changes, emit the 'input' event with the file data.
      this.$emit('input', e.target.files[0])
    },
    clearSelected() {
        console.log(this.value);
        this.$emit('input', null);
    }
  }
}
</script>