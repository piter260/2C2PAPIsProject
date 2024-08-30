<template>
    <!--<div>
        <h1>{{ msg }}</h1>
        <button @click="fetchWeather">Fetch Weather</button>
        <ul>
            <li v-for="forecast in weather" :key="forecast.date">
                {{ forecast.date }} - {{ forecast.summary }} - {{ forecast.temperatureC }}°C
            </li>
        </ul>
    </div>-->
</template>

<script>
import axios from 'axios';
export default {
  name: 'HelloWorld',
  props: {
    msg: String
  },
   methods: {
    async fetchWeather() {
      try {
        // Replace with your actual API endpoint URL
          const response = await axios.get('https://localhost:7202/WeatherForecast');
          console.log('fetchWeather response ',response);
        this.weather = response.data;
      } catch (error) {
        console.error('Error fetching weather data:', error);
      }
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
h3 {
  margin: 40px 0 0;
}
ul {
  list-style-type: none;
  padding: 0;
}
li {
  display: inline-block;
  margin: 0 10px;
}
a {
  color: #42b983;
}
</style>
