<template>
    <div>
        <h1>{{ msg }}</h1>
        <button @click="fetchWeather">Fetch Weather</button>
        <ul>
            <li v-for="forecast in weather" :key="forecast.date">
                {{ forecast.date }} - {{ forecast.summary }} - {{ forecast.temperatureC }}°C
            </li>
        </ul>
        <div>
            <h1>Upload File</h1>
            <input type="file" @change="onFileChange" />
            <button @click="uploadFile">Upload</button>
        </div>
    </div>
</template>

<script>
    import axios from 'axios';
    export default {
        name: 'UploadFileComponent',
        data() {
            return {
                selectedFile: null
            };
        },
        methods: {
            async fetchWeather() {
                try {
                    // Replace with your actual API endpoint URL
                    const response = await axios.get('https://localhost:7202/WeatherForecast');
                    console.log('fetchWeather response ', response);
                    this.weather = response.data;
                } catch (error) {
                    console.error('Error fetching weather data:', error);
                }
            },
            onFileChange(event) {
                this.selectedFile = event.target.files[0];
            },
            async uploadFile() {
                if (!this.selectedFile) {
                    alert("Please select a file first");
                    return;
                }

                const formData = new FormData();
                formData.append("file", this.selectedFile);

                try {
                    const response = await axios.post("https://localhost:7202/UploadFiles/FileUpload", formData, {
                        headers: {
                            "Content-Type": "multipart/form-data"
                        }
                    });
                    console.log("File uploaded successfully:", response.data);
                } catch (error) {
                    console.error("Error uploading file:", error);
                }
            }
        }
    }
</script>

<style scoped>
</style>