<template>
    <div>
        <h1>Coord System Upload</h1>
        <div>
            <v-alert type="success" v-model="uploaded" dismissible outlined>
                Coordinate System Uploaded Successfully
            </v-alert>
            <v-alert type="error" v-model="uploadFailed" dismissible outlined>
                Coordinate System Failed to Upload - See Developer Tools
            </v-alert>
            <v-card style="height: 400px;"  v-if="uploading">
                <v-layout
                    align-content-center
                    justify-center
                    fill-height
                    wrap
                >
                <v-flex
                    xs12
                    subtitle-1
                    text-center
                >
                    Uploading Coordinate System
                </v-flex>
                <v-flex xs6>
                    <v-progress-linear
                    color="green accent-4"
                    indeterminate
                    rounded
                    height="6"
                    ></v-progress-linear>
                </v-flex>
                </v-layout>
            </v-card>
        </div>
        <div v-if="!uploading">
            <p>
                <file-select v-model="file" prompt="Step 1: Select a Coordinate System" :disabled="uploading"></file-select>
            </p>

            <!-- stage 2 - project UID -->
            <p>
                <v-card outlined :disabled="!file || uploading">
                    <v-list-item two-line>
                        <v-list-item-content>
                            <v-list-item-title class="overline mb-4">Step 2: Enter Project UID</v-list-item-title>
                            <v-text-field
                                v-model="projectUid"
                                :rules="projectUidRules"
                                label="Project UID"
                                required
                                ></v-text-field>
                        </v-list-item-content>
                    </v-list-item>
                </v-card>
            </p>

            <!-- stage 3 - project UID -->
            <p>
                <v-card outlined :disabled="!file || !projectUid || uploading">
                    <v-list-item two-line>
                        <v-list-item-content>
                            <v-list-item-title class="overline mb-4">Step 3: Upload Coordinate System</v-list-item-title>
                        </v-list-item-content>
                    </v-list-item>
                    <v-card-actions>
                        <v-btn class="select-button ma-2" color="primary" v-on:click="greet">Upload</v-btn>
                    </v-card-actions>
                </v-card>
            </p>
        </div>
    </div>
</template>

<script>
import FileSelect from '../../components/FileSelect.vue'
import Consts from '../../const.js'
import axios from "axios";
var urljoin = require('url-join');

export default {
  components: {
    FileSelect
  },
  data() {
    return {
      uploaded: false,
      uploadFailed: false,
      uploading: false,
      file: null,
      projectUid: null,
      projectUidRules: [
        v => !!v || 'A Project UID must be provided.',
        v => Consts.GUID_REGEX.test(v) || 'Project UID must be a valid UID.',
      ],
    }
  },
  methods: {
    greet: function (event) {
        var self = this;
        this.uploading = true;
        this.base64Encode(this.file).then((encoded) => {
            var payload = {
                "projectUid" : this.projectUid,
                "csFileName" : self.file.name,
                "csFileContent" : encoded
            };
            var url = urljoin(process.env.VUE_APP_TREX_MUTABLE_GATEWAY_URL, "/api/v1/coordsystem");
            axios.post(url, payload, {
                headers: {
                        "Content-Type": "application/json"
                    }
                })
                .then(result => {
                    console.log("result", result);
                    this.uploading = false;
                    this.file = null;
                    this.projectUid = null;
                    this.uploaded = true;
                })
                .catch(error => {
                    console.log("fetch-error", error);
                    this.uploading = false;
                    this.uploadFailed = true;
                });
        }).catch(error => {
            console.log("encoding error", error);
            this.uploading = false;
            this.uploadFailed = true;
        })
    }
  }
}
</script>