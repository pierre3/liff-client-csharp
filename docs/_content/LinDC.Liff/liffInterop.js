window.liffInterop = {
    init: function () {
        return new Promise(function (resolve, reject) {
            if (!liff) {
                reject("LIFF object not ready.");
                return;
            }
            liff.init(
                function (data) {
                    resolve(JSON.stringify(data));
                },
                function (error) {
                    reject(error);
                });
        });
    }
};