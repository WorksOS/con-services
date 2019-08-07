

export default {
    methods : {
        base64Encode: (file) => {
            return new Promise((resolve, reject) => {
                var reader = new FileReader()
                reader.readAsDataURL(file)
                reader.onload = ()=> {
                    let encoded = reader.result.replace(/^data:(.*,)?/, '');
                    if ((encoded.length % 4) > 0) {
                        encoded += '='.repeat(4 - (encoded.length % 4));
                    }
                    resolve({file, encoded});
                };
                reader.onerror = error => reject(error);
            });
        }
    }
};