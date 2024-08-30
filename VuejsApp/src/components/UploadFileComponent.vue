<template>
    <div>
        <h1>{{ msg }}</h1>
        <button @click="fetchWeather">Fetch Weather</button>
        <ul>
            <li v-for="forecast in weather" :key="forecast.date">
                {{ forecast.date }} - {{ forecast.summary }} - {{ forecast.temperatureC }}°C
            </li>
        </ul>
    </div>
</template>

<script>
    import axios from 'axios';
    export default {
        name: 'UploadFileComponent',
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
            }
        }
    }
</script>

<style scoped>
</style>